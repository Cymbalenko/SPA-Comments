import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

export interface CommentDto {
  id: string;
  userName: string;
  email: string;
  homePage?: string;
  text: string;
  createdAt: string;
  parentId?: string | null;
  files?: Array<{ id: string; url: string; fileName: string; contentType: string }>;
  childrenCount?: number;
  replies?: CommentDto[];
}

@Injectable({ providedIn: 'root' })
export class CommentsService {
  private apiUrl = '/api/comment';

  constructor(private http: HttpClient) { }

  // Получение топовых комментариев с пагинацией и сортировкой
  getTopComments(page: number, pageSize: number, sortField: string, sort: 'ASC' | 'DESC'): Observable<any> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('sortField', sortField)
      .set('sort', sort);

    return this.http.get<{ total: number; items: CommentDto[] }>(`${this.apiUrl}/getCommentList`, { params }).pipe(
      map((response) => response)
    );
  }

  // Получение дочерних комментариев
  getCommentChildren(parentId: string): Observable<CommentDto[]> {
    const params = new HttpParams().set('parentId', parentId);
    return this.http.get<CommentDto[]>(`${this.apiUrl}/children`, { params }).pipe(
      map((response) => response)
    );
  }

  // Добавление комментария
  addComment(input: {
    userName: string;
    email: string;
    homePage?: string;
    text: string;
    parentId?: string | null;
    captchaToken: string;
    files?: File[];
  }): Observable<any> {
    const formData = new FormData();
    formData.append('userName', input.userName);
    formData.append('email', input.email);
    formData.append('homePage', input.homePage || '');
    formData.append('text', input.text);
    formData.append('parentId', input.parentId || '');
    formData.append('captcha', input.captchaToken);

    if (input.files) {
      input.files.forEach((file, index) => {
        formData.append('files', file, file.name);
      });
    }

    return this.http.post(`${this.apiUrl}/create`, formData).pipe(
      map((response) => response)
    );
  }
}
