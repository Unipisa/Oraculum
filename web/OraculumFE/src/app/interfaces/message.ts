import { SafeHtml } from "@angular/platform-browser";

export interface Message {
  id: string;
  text: any;
  sender: string;
  timestamp: string;
  extraFactIds?: any;
  factIds?: any;
  feedback?: boolean;
  feedbackText?: string;
  completed?: boolean;
  url?: string;
}
