import { ChangeDetectorRef, AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { CommentsService } from '../../services/comment.service';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { merge, of } from 'rxjs';
import { startWith, switchMap, map, catchError } from 'rxjs/operators';

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

  constructor(private svc: CommentsService, private cdRef: ChangeDetectorRef) { }

  ngOnInit() { }

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
          const sort = (this.sort.direction || 'desc').toUpperCase();
          return this.svc.getTopComments(page, pageSize, sortField, sort as 'ASC' | 'DESC');
        }),
        map(result => {
          console.log('Loaded comments result:', result);
          this.isLoading = false;
          this.total = result.totalCount;
          return result.data;
        }),
        catchError(err => {
          console.error(err);
          this.isLoading = false;
          this.errorMsg = 'Ошибка загрузки комментариев';
          return of([]);
        })
      )
      .subscribe(items => {
        console.log('Loaded comments:', items);
        this.dataSource.data = items;
        this.cdRef.detectChanges();
      });
  }

  toggleReplies(comment: any) {
    comment.showReplies = !comment.showReplies;
  }

  showReplyForm(comment: any) {
    // Show the reply form, you may need a separate form component or method
    comment.showReplyForm = !comment.showReplyForm;
  }

  onReplyAdded() {
    this.paginator._changePageSize(this.paginator.pageSize);  // Refresh current page
  }

  onCommentSubmitted() {
    this.paginator._changePageSize(this.paginator.pageSize);  // Reload data after submitting a new comment
  }
}
