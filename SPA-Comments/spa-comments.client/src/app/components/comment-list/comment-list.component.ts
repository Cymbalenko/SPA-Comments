import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  OnInit,
  ViewChild,
} from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort, Sort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { merge, of } from 'rxjs';
import { catchError, map, startWith, switchMap } from 'rxjs/operators';
import { CommentsGraphqlService } from '../../services/graphql.service';
import { CommentSignalRService } from '../../services/comment-signalr.service';
import { FileService } from '../../services/file.service';
import DOMPurify from 'dompurify';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-comment-list',
  templateUrl: './comment-list.component.html',
  styleUrls: ['./comment-list.component.scss']
})
export class CommentListComponent implements OnInit, AfterViewInit {
  displayedColumns = ['expand', 'userName', 'email', 'createdAt', 'text'];
  dataSource = new MatTableDataSource<any>();
  total = 0;
  pageSize = 25;
  isLoading = false;
  errorMsg?: string;
  selectedComment: any = null;
  currentSort: { field: string; dir: 'asc' | 'desc' } = {
    field: 'createdAt',
    dir: 'desc',
  };
  showNewCommentForm = false;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private svc: CommentsGraphqlService,
    private signalR: CommentSignalRService,
    private cdRef: ChangeDetectorRef,
    private fileService: FileService,
    private sanitizer: DomSanitizer
  ) { }

  ngOnInit() {
    this.signalR.startConnection();

    this.signalR.newComment$.subscribe((newComment: any) => {
      if (!newComment) return;
      this.replaceFileUrls(newComment);
      this.addCommentToTree(newComment);
      this.cdRef.detectChanges();
    });
  }

  ngAfterViewInit() {
    merge(this.paginator.page, this.sort.sortChange)
      .pipe(
        startWith({}),
        switchMap(() => {
          this.isLoading = true;
          const page = (this.paginator.pageIndex || 0) + 1;
          const pageSize = this.paginator.pageSize || this.pageSize;
          const sortField = this.sort?.active || this.currentSort.field;
          const sortDir = (this.sort?.direction || this.currentSort.dir) as 'asc' | 'desc';
          this.currentSort = { field: sortField, dir: sortDir };
          return this.svc.getParentComments(page, pageSize, sortField, sortDir);
        }),
        map(result => {
          this.isLoading = false;
          this.total = result.totalCount || 0;
          return this.mapComments(result.data || []);
        }),
        catchError(err => {
          this.isLoading = false;
          this.errorMsg = 'Ошибка загрузки комментариев';
          return of([]);
        })
      )
      .subscribe(items => {
        this.dataSource.data = items;
        this.cdRef.detectChanges();
      });
  }

  private mapComments(comments: any[]): any[] {
    return comments.map(c => { 

      c.safeHtml = this.sanitizer.bypassSecurityTrustHtml(
        DOMPurify.sanitize(c.text ?? '', {
          ALLOWED_TAGS: ['b', 'i', 'strong', 'a', 'code', 'pre', 'u', 'p', 'br'],
          ALLOWED_ATTR: ['href', 'target', 'title']
        })
      );
      this.replaceFileUrls(c);
      c.hasReplies = c.repliesCount && c.repliesCount > 0;
      c.replies = undefined;

      return c;
    });
  }

  replaceFileUrls(comment: any): void {
    if (!comment.files?.length) return;

    comment.files.forEach((file: any, index: number) => {
      const match = file.uri?.match(/comment-files\/(.+)$/);
      const blobName = match ? match[1] : null; 
      if (!blobName) return;

      // Сохраняем оригинальный URI, чтобы потом заменить
      const originalUri = file.uri;

      this.fileService.getDownloadUrl(blobName).subscribe({
        next: (response: any) => {
          const publicUrl = response.publicUrl;

          // Если URL изменился — обновляем и принудительно триггерим
          if (file.uri !== publicUrl) {
            file.uri = publicUrl; 

            // Критично: принудительно обновляем вид
            this.cdRef.detectChanges();
          }
        },
        error: (err) => {
          console.error('Failed to get public URL:', err);
          // Оставляем оригинальный URI
          file.uri = originalUri;
        }
      });
    });
  }

  async onRowClick(row: any) {
    // Если тот же комментарий — просто свернуть
    if (this.selectedComment === row) {
      this.selectedComment = null;
      return;
    }

    this.isLoading = true;

    try { 
      if (row.replies === undefined) {
        const fullReplies = await this.loadRepliesRecursive(row.id);
        row.replies = fullReplies;
        this.dataSource.data = [...this.dataSource.data];
      }
       
      this.selectedComment = row;
    } catch (err) {
      console.error('Ошибка загрузки дерева комментариев:', err);
    } finally {
      this.isLoading = false;
      this.cdRef.detectChanges();
    }
  }

  private async loadRepliesRecursive(commentId: number): Promise<any[]> {
    try {
      const replies = await this.svc.getReplies(commentId).toPromise();

      const mapped = this.mapComments(replies || []);

      for (const reply of mapped) {
        if (reply.hasReplies || (reply.repliesCount && reply.repliesCount > 0)) {
          reply.replies = await this.loadRepliesRecursive(reply.id);
        } else {
          reply.replies = [];
        }
      }

      return mapped;
    } catch (err) {
      console.error('Ошибка загрузки ответов:', err);
      return [];
    }
  }


  private addCommentToTree(newComment: any) {
    if (!newComment.parentId) {
      this.dataSource.data = [newComment, ...this.dataSource.data];
    } else {
      const parent = this.findParent(this.dataSource.data, newComment.parentId);
      if (parent) {
        parent.replies ??= [];
        parent.replies.push(newComment);
        this.dataSource.data = [...this.dataSource.data];
      }
    }
  }

  private findParent(comments: any[], parentId: string): any | null {
    for (const c of comments) {
      if (c.id === parentId) return c;
      if (c.replies?.length) {
        const found = this.findParent(c.replies, parentId);
        if (found) return found;
      }
    }
    return null;
  }

  showReplyForm(row: any) {
    this.selectedComment = this.selectedComment === row ? null : row;
    this.cdRef.detectChanges();
  }

  reload() {
    if (this.paginator) this.paginator.firstPage();
    this.isLoading = true;

    this.svc.getParentComments(1, this.pageSize, this.currentSort.field, this.currentSort.dir).subscribe({
      next: result => {
        this.isLoading = false;
        this.total = result.totalCount || 0;
        this.dataSource.data = this.mapComments(result.data || []);
        this.cdRef.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.errorMsg = 'Ошибка загрузки комментариев';
      }
    });
  }

  onSortChange(event: Sort) {
    this.currentSort = { field: event.active, dir: event.direction as any || 'desc' };
  }

  getCommentPreview(text: string): SafeHtml {
    if (!text) return '';
    const preview = text.length > 120 ? text.slice(0, 120) + '…' : text;
    return this.sanitizer.bypassSecurityTrustHtml(preview);
  }

  onReplySubmitted(newReply?: any) {
    if (!newReply) return;
    this.addCommentToTree(newReply);
    this.selectedComment = null;
    this.cdRef.detectChanges();
  }
}
