import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';

export interface CommentDto {
  id: string;
  userName: string;
  email: string;
  homePage?: string;
  text: string;
  createdAt: string;
  parentId?: string | null;
  replies?: CommentDto[];
}

@Injectable({ providedIn: 'root' })
export class CommentsGraphqlService {
  private readonly graphqlUrl = '/graphql'; // точка входа на бэке

  constructor(private http: HttpClient) { }

  /**
   * Получение родительских комментариев с вложенными ответами
   */
  getParentComments(
    page: number = 1,
    pageSize: number = 25,
    sortField: string = 'userName',
    sort: 'asc' | 'desc' = 'desc'
  ): Observable<any> {
    const query = `
    query parentComments($page: Int!, $pageSize: Int!, $sortField: String!, $sort: String!) {
      parentComments(page: $page, pageSize: $pageSize, sortField: $sortField, sort: $sort) {
        data {
          id
          userName
          email
          homePage
          text
          parentId
          createdAt
          files {
            id
            name
            contentType
            uri
            isImage
            uploadedAt
          }
        }
        totalCount
        page
        pageSize
      }
    }`;

    const variables = { page, pageSize, sortField, sort };

    return this.http
      .post<{ data: any }>(this.graphqlUrl, { query, variables })
      .pipe(map((response) => response.data.parentComments));
  }

  /**
   * Добавление комментария (GraphQL mutation)
   */
  addComment(input: {
    userName: string;
    email: string;
    homePage?: string;
    text: string;
    parentId?: string | null;
    captchaToken: string;
  }): Observable<any> {
    const mutation = `
      mutation AddComment($input: AddCommentInput!) {
        addComment(input: $input) {
          id
          userName
          email
          text
          createdAt
          parentId
        }
      }`;

    const variables = { input };

    return this.http
      .post<{ data: any }>(this.graphqlUrl, { query: mutation, variables })
      .pipe(map((response) => response.data.addComment));
  }

  /**
   * Получение дочерних комментариев по parentId
   */
  getReplies(parentId: number): Observable<CommentDto[]> {
    const query = `
    query replies($parentId: Int!) {
      replies(parentId: $parentId) {
        id
        userName
        email
        homePage
        text
        createdAt
        parentId
        repliesCount
        files {
          id
          name
          contentType
          uri
          isImage
          uploadedAt
        }
      }
    }`;

    return this.http
      .post<{ data: { replies: CommentDto[] } }>(this.graphqlUrl, { query, variables: { parentId } })
      .pipe(map(res => res.data.replies));
  }

  }
