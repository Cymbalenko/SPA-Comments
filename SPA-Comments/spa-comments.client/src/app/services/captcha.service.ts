import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class CaptchaService {
  private baseUrl = '/api/captcha';

  constructor(private http: HttpClient, private sanitizer: DomSanitizer) { }

  getCaptchaImage(): Observable<SafeUrl> {
    return new Observable((observer) => {
      this.http
        .get(`${this.baseUrl}/image`, { responseType: 'blob', withCredentials: true })
        .subscribe({
          next: (blob) => {
            const url = URL.createObjectURL(blob);
            observer.next(this.sanitizer.bypassSecurityTrustUrl(url));
            observer.complete();
          },
          error: (err) => observer.error(err),
        });
    });
  }

  validateCaptcha(value: string): Observable<boolean> {
    return this.http.post<boolean>(
      `${this.baseUrl}/validate`,
      { value },
      { withCredentials: true }
    );
  }
}
