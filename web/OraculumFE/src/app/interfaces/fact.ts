export interface Fact {
  category: string;
  citation?: string;
  content: string;
  expiration: string | null;
  outOfLimit?: any;
  factType: string;
  id: string;
  reference?: string;
  tags?: string[];
  title: string;
  distance?: number;
  score?: number;
  feedback?: boolean;
}
