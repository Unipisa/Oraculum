export interface Chat {
  id: string,
  name?: string,
  sibyllaId: string,
  sibyllaRef?: string,
  messagesIds?: string[],
  messages?: any,
  isActive: boolean
}
