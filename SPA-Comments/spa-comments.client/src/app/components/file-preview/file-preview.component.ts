import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-file-preview',
  templateUrl: './file-preview.component.html',
  styleUrls: ['./file-preview.component.scss']
})
export class FilePreviewComponent {
  textCache?: string;

  constructor(
    public dialogRef: MatDialogRef<FilePreviewComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { id: string; url: string; fileName: string; contentType: string },
    private http: HttpClient
  ) {}

  close() {
    this.dialogRef.close();
  }
  loadText(): string {
    if (this.data.contentType !== 'text/plain') return '';
    if (this.textCache !== undefined) return this.textCache;
    // синхронно вернуть placeholder, асинхронно загрузить и сохранить
    this.textCache = 'Loading...';
    this.http.get(this.data.url, { responseType: 'text' }).subscribe({
      next: txt => { this.textCache = txt; },
      error: () => { this.textCache = 'Cannot load text file.'; }
    });
    return this.textCache;
  }
}
