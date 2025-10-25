import {
  ChangeDetectorRef,
  AfterViewInit,
  Component,
  OnInit,
  ViewChild
} from '@angular/core';
import { CommentsGraphqlService } from '../../services/graphql.service';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { merge, of } from 'rxjs';
import { startWith, switchMap, map, catchError } from 'rxjs/operators';
import { CommentSignalRService } from '../../services/comment-signalr.service';

@Component({
  selector: 'app-comment-list',
  templateUrl: './comment-list.component.html',
  styleUrls: ['./comment-list.component.scss']
})
export class CommentListComponent implements OnInit, AfterViewInit {
  displayedColumns = ['userName', 'email', 'createdAt', 'actions'];
  dataSource = new MatTableDataSource<any>();
  total = 0;
  pageSize = 25;
  isLoading = false;
  errorMsg?: string;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    private svc: CommentsGraphqlService,
    private signalR:CommentSignalRService,
    private cdRef: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.signalR.startConnection();
    this.signalR.newComment$.subscribe(comment => {
      if (comment) {
        console.log('Adding new comment to table', comment);
        this.dataSource.data = [comment, ...this.dataSource.data];
        this.cdRef.detectChanges();
      }
    });
  }

  ngAfterViewInit() {
    this.sort.active = 'createdAt';
    this.sort.direction = 'desc';
    this.paginator.pageSize = this.pageSize;

    merge(this.sort.sortChange, this.paginator.page)
      .pipe(
        startWith({}),
        switchMap(() => {
          this.isLoading = true;
          this.errorMsg = undefined;

          const page = (this.paginator.pageIndex || 0) + 1;
          const pageSize = this.paginator.pageSize || this.pageSize;
          const sortField = this.sort.active || 'createdAt';
          const sort = (this.sort.direction || 'desc').toLowerCase() as
            | 'asc'
            | 'desc';

          return this.svc.getParentComments(page, pageSize, sortField, sort);
        }),
        map(result => {
          this.isLoading = false;
          this.total = result.totalCount || 0;
          return result.data || [];
        }),
        catchError(err => {
          console.error('GraphQL error:', err);
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

  toggleReplies(comment: any) {
    comment.showReplies = !comment.showReplies;
  }

  showReplyForm(comment: any) {
    comment.showReplyForm = !comment.showReplyForm;
  }

  onReplyAdded() {
    this.paginator._changePageSize(this.paginator.pageSize);
  }

  onCommentSubmitted() {
    this.paginator._changePageSize(this.paginator.pageSize);
  }
}
