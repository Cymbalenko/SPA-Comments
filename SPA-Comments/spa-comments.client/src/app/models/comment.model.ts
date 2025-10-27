export interface Comment {
  id: number;
  userName: string;
  email: string;
  homePage?: string;
  text: string;
  createdAt: string;
  parentId?: number;
  fileUrl?: string;
  files?: any[];
  replies?: Comment[];
  repliesCount: number;
}
export interface CommentDto {
  id: number;
  userName: string;
  email: string;
  homePage?: string;
  text: string;
  createdAt: string;
  parentId?: number;
  files?: any[];
  children?: CommentDto[];
  hasLoadedReplies?: boolean; 
}
