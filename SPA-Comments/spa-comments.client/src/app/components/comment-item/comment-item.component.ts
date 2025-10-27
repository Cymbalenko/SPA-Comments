import { Component, Input, Output, EventEmitter } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatDialog } from '@angular/material/dialog';
import { HttpClient } from '@angular/common/http';
import { FileService } from '../../services/file.service';

@Component({
  selector: 'app-comment-item',
  templateUrl: './comment-item.component.html',
  styleUrls: ['./comment-item.component.scss']
})
export class CommentItemComponent {
  @Input() comment: any;
  @Output() replyAdded = new EventEmitter<any>(); // событие нового ответа
  showReplyForm = false;
  safeHtml: SafeHtml = '';

  constructor(private sanitizer: DomSanitizer,
    private dialog: MatDialog,
    private http: HttpClient,
    private fileService: FileService
  ) { }

  ngOnInit() {
    this.safeHtml = this.sanitizer.bypassSecurityTrustHtml(this.comment.text);
    if (this.comment.files?.length) {
      for (const f of this.comment.files) {
        if (!f.isImage) this.loadTextPreview(f);
      }
    }
  }

  loadTextPreview(file: any) {
    if (!file.uri.endsWith('.txt')) return;

    const match = file.uri.match(/comment-files\/(.+)$/);
    const blobName = match ? match[1] : null;
    if (!blobName) {
      file.previewText = 'Ошибка: имя файла';
      return;
    }

    // Используем кэшированный SAS URL или получаем новый
    if (file._sasUrl) {
      this.fetchPreview(file);
    } else {
      this.fileService.getDownloadUrl(blobName).subscribe({
        next: (res: any) => {
          file._sasUrl = res.publicUrl;
          this.fetchPreview(file);
        },
        error: () => file.previewText = 'Ошибка загрузки'
      });
    }
  }

  private fetchPreview(file: any) {
    fetch(file._sasUrl)
      .then(r => {
        if (!r.ok) throw new Error();
        return r.text();
      })
      .then(text => {
        file.previewText = text;
      })
      .catch(() => {
        file.previewText = 'Не удалось загрузить';
      });
  }

  onReplySubmitted(newReply: any) {
    this.showReplyForm = false;
    if (newReply) {
      this.replyAdded.emit(newReply); // передаем новый комментарий наверх
    }
  }
}
