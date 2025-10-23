import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommentsService } from '../../services/comment.service';
import DOMPurify from 'dompurify';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

const URL_REGEX = /^(https?:\/\/)?([\w-]+\.)+[\w-]+(\/[\w\-._~:?#[\]@!$&'()*+,;=]*)?$/;

@Component({
  selector: 'app-comment-form',
  templateUrl: './comment-form.component.html',
  styleUrls: ['./comment-form.component.scss']
})
export class CommentFormComponent implements OnInit {
  @Input() parentId?: string | null = null; // for reply
  @Output() submitted = new EventEmitter<void>();
  captchaUrl!: string;
  form!: FormGroup;
  previewHtml?: SafeHtml;
  uploading = false;
  selectedFiles: File[] = [];
  previewFiles: Array<{ type: 'image' | 'text', url: string, name: string }> = [];

  constructor(
    private fb: FormBuilder,
    private commentsService: CommentsService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit() {
    this.captchaUrl = this.getCaptchaUrl();
    this.form = this.fb.group({
      userName: ['', [Validators.required, Validators.pattern(/^[A-Za-z0-9_-]{2,30}$/)]],
      email: ['', [Validators.required, Validators.email]],
      homePage: ['', [Validators.pattern(URL_REGEX)]],
      captcha: ['', Validators.required],
      text: ['', [Validators.required, Validators.minLength(3)]]
    });
  }

  // Toolbar helpers: insert tag around selection
  insertTag(tag: string) {
    const el: HTMLTextAreaElement | null = document.querySelector('#comment-text') as any;
    if (!el) return;
    const start = el.selectionStart ?? 0;
    const end = el.selectionEnd ?? 0;
    const val = this.form.value.text || '';
    const before = val.slice(0, start);
    const selected = val.slice(start, end);
    const after = val.slice(end);
    const wrapped = `<${tag}>${selected || ''}</${tag}>`;
    const newVal = before + wrapped + after;
    this.form.controls['text'].setValue(newVal);
    // restore focus
    setTimeout(() => {
      el.focus();
      const pos = start + tag.length + 2 + (selected ? selected.length : 0);
      el.setSelectionRange(pos, pos);
    }, 0);
  }

  // Preview sanitized HTML
  preview() {
    const raw = this.form.value.text || '';
    const sanitized = DOMPurify.sanitize(raw, {
      ALLOWED_TAGS: ['a', 'code', 'i', 'strong'],
      ALLOWED_ATTR: ['href', 'title', 'target']
    });
    this.previewHtml = this.sanitizer.bypassSecurityTrustHtml(sanitized);
  }
  refreshCaptcha() {
    // Implement the refresh logic for the captcha
    console.log('Captcha refreshed');
  }
  onFileSelected(ev: Event) {
    const input = ev.target as HTMLInputElement;
    if (!input.files) return;
    Array.from(input.files).forEach(file => {
      if (file.type.startsWith('image/')) {
        // will process resize later
        this.processImageFile(file);
      } else if (file.type === 'text/plain') {
        if (file.size > 100 * 1024) {
          alert('Text file too big (max 100KB)');
          return;
        }
        const reader = new FileReader();
        reader.onload = () => {
          this.previewFiles.push({ type: 'text', url: reader.result as string, name: file.name });
        };
        reader.readAsText(file);
        this.selectedFiles.push(file);
      } else {
        alert('Unsupported file type');
      }
    });
    // clear input
    input.value = '';
  }

  // Resize image to max 320x240 keeping aspect ratio
  private processImageFile(file: File) {
    const img = new Image();
    const reader = new FileReader();
    reader.onload = () => {
      img.src = reader.result as string;
    };
    img.onload = () => {
      const maxW = 320, maxH = 240;
      let { width: w, height: h } = img;
      let scale = Math.min(1, Math.min(maxW / w, maxH / h));
      const canvas = document.createElement('canvas');
      canvas.width = Math.round(w * scale);
      canvas.height = Math.round(h * scale);
      const ctx = canvas.getContext('2d')!;
      ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
      canvas.toBlob((blob) => {
        if (!blob) return;
        const newFile = new File([blob], file.name, { type: blob.type });
        this.selectedFiles.push(newFile);
        const url = URL.createObjectURL(blob);
        this.previewFiles.push({ type: 'image', url, name: newFile.name });
      }, file.type, 0.9);
    };
    reader.readAsDataURL(file);
  }

  removePreview(idx: number) {
    this.previewFiles.splice(idx, 1);
    this.selectedFiles.splice(idx, 1);
  }

  getCaptchaUrl(): string {
    return `/api/captcha-image?ts=${new Date().getTime()}`;
  }

  async onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.uploading = true;

    // sanitize text server-side is required, but send sanitized HTML as well
    const raw = this.form.value.text || '';
    const sanitized = DOMPurify.sanitize(raw, {
      ALLOWED_TAGS: ['a', 'code', 'i', 'strong'],
      ALLOWED_ATTR: ['href', 'title', 'target']
    });

    const input = {
      userName: this.form.value.userName,
      email: this.form.value.email,
      homePage: this.form.value.homePage || null,
      text: sanitized,
      parentId: this.parentId || null,
      captchaToken: this.form.value.captcha
    };

    try {
      const res = await this.commentsService.addComment({
        ...input,
        files: this.selectedFiles
      }).toPromise();
      // success
      this.form.reset();
      this.previewHtml = undefined;
      this.selectedFiles = [];
      this.previewFiles = [];
      this.uploading = false;
      this.submitted.emit();
    } catch (err) {
      console.error(err);
      alert('Error submitting comment');
      this.uploading = false;
    }
  }
}
