import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class FileService {
  private apiUrl = '/api/file';

  constructor(private http: HttpClient) { }

  /** Получить публичную ссылку по имени blob (blobName) */
  getPublicUrl(blobName: string): Observable<string> {
    const params = new HttpParams().set('blobName', blobName);
    return this.http
      .get<{ url: string }>(`${this.apiUrl}/public-url`, { params })
      .pipe(map(response => response.url));
  }

  getDownloadUrl(blobName: string): Observable<any> {
    const params = new HttpParams().set('blobName', blobName);
    return this.http
      .get<{ url: any }>('/api/file/download-url', { params })
      .pipe(map(r => r));
  }
}
