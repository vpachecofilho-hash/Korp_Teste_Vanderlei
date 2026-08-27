import { InvoiceItem } from './invoice-item';

export interface Invoice {
  id: string;
  number: number;
  status: 'Open' | 'Closed';
  createdAtUtc: string;
  items: InvoiceItem[];
}