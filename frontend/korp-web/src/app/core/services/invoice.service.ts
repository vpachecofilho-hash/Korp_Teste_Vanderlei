import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Invoice } from '../../models/invoice';
import { CreateInvoiceRequest } from '../../models/requests';

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {

  private readonly apiUrl = 'http://localhost:5102/api/invoices';

  constructor(private http: HttpClient) {}

  getAll(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(this.apiUrl);
  }

  getById(id: string): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.apiUrl}/${id}`);
  }

  create(request: CreateInvoiceRequest): Observable<Invoice> {
    return this.http.post<Invoice>(this.apiUrl, request);
  }

  close(id: string): Observable<Invoice> {
    return this.http.post<Invoice>(
      `${this.apiUrl}/${id}/close`,
      {}
    );
  }
}