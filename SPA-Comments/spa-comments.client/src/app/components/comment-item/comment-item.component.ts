import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommentsService, CommentDto } from '../../services/comment.service';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import DOMPurify from 'dompurify';
import { MatDialog } from '@angular/material/dialog';
import { FilePreviewComponent } from '../file-preview/file-preview.component';

@Component({
  selector: 'app-comment-item',
  templateUrl: './comment-item.component.html',
  styleUrls: ['./comment-item.component.scss']
})
export class CommentItemComponent implements OnInit {
  @Input() comment!: CommentDto;
  @Output() refresh = new EventEmitter<void>();

  safeHtml?: SafeHtml;
  showReplyForm = false;
  children: CommentDto[] = [];
  loadingChildren = false;
  
  constructor(private svc: CommentsService, private sanitizer: DomSanitizer, private dialog: MatDialog) {}

  ngOnInit() {
    this.safeHtml = this.sanitizer.bypassSecurityTrustHtml(
      DOMPurify.sanitize(this.comment.text, { ALLOWED_TAGS: ['a', 'code', 'i', 'strong'], ALLOWED_ATTR: ['href', 'title', 'target'] })
    );
  }

  toggleChildren() {
    if (this.children.length) {
      this.children = [];
      return;
    }
    this.loadingChildren = true;
    this.svc.getCommentChildren(this.comment.id).subscribe({
      next: items => {
        this.children = items;
        this.loadingChildren = false;
      },
      error: () => {
        this.loadingChildren = false;
      }
    });
  }
  
  openPreview(file: { id: string; url: string; fileName: string; contentType: string }) {
    this.dialog.open(FilePreviewComponent, {
      data: file,
      width: '80vw',
      maxWidth: '900px'
    });
  }
  onReplySubmitted() {
    this.showReplyForm = false;
    this.refresh.emit();
  }
}
