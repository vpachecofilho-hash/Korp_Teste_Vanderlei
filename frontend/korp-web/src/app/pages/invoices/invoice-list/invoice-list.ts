import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

import { Invoice } from '../../../models/invoice';
import { InvoiceService } from '../../../core/services/invoice.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-invoice-list',
  imports: [
    CommonModule,
    RouterLink
  ],
  templateUrl: './invoice-list.html',
  styleUrl: './invoice-list.scss'
})
export class InvoiceList implements OnInit {

  invoices: Invoice[] = [];
  loading = false;
  errorMessage = '';

  constructor(
    private invoiceService: InvoiceService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    this.loading = true;
    this.errorMessage = '';

    this.invoiceService.getAll()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: invoices => {
          this.invoices = invoices;
        },

        error: error => {
          console.error(error);

          this.errorMessage =
            'Não foi possível carregar as notas.';
        }
      });
  }
}