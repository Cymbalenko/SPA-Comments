export interface Comment {
  id: string;
  userName: string;
  email: string;
  homePage?: string;
  text: string;
  createdAt: string;
  parentId?: string;
  fileUrl?: string;
  replies?: Comment[];
}
