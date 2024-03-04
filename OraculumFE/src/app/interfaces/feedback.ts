// interfaces/feedback.ts

export interface FeedbackBase {
  text?: string;
  rating: string;
}

export interface FeedbackWithMessageId extends FeedbackBase {
  messageId: string;
}

export interface FeedbackWithFactId extends FeedbackBase {
  factId: string;
}

export type FeedbackOptions = {
  chatId?: string;
  factId?: string;
} & (FeedbackBase | FeedbackWithMessageId | FeedbackWithFactId);
