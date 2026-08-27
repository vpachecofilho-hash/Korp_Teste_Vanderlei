import { Routes } from '@angular/router';

import { ProductList }
  from './pages/products/product-list/product-list';

import { ProductForm }
  from './pages/products/product-form/product-form';

import { InvoiceList }
  from './pages/invoices/invoice-list/invoice-list';

import { InvoiceForm }
  from './pages/invoices/invoice-form/invoice-form';

import { InvoiceDetail }
  from './pages/invoices/invoice-detail/invoice-detail';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'products',
    pathMatch: 'full'
  },

  {
    path: 'products',
    component: ProductList
  },

  {
    path: 'products/new',
    component: ProductForm
  },

  {
    path: 'invoices',
    component: InvoiceList
  },
  {
    path: 'invoices/new',
    component: InvoiceForm
  },
  {
    path: 'invoices/:id',
    component: InvoiceDetail
  }
];