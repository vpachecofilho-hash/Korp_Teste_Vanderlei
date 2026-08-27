import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './product-form.html'
})
export class ProductForm {

  loading = false;
  message = '';

  form;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private cdr: ChangeDetectorRef
  ) {
    this.form = this.fb.group({
      code: ['', Validators.required],
      description: ['', Validators.required],
      stock: [0, [Validators.required, Validators.min(0)]]
    });
  }

  save(): void {
    if (this.form.invalid) {
      return;
    }

    const value = this.form.getRawValue();

    this.loading = true;

    this.productService.create({
      code: value.code!,
      description: value.description!,
      stock: value.stock!
    }).subscribe({
      next: () => {
        this.loading = false;
        this.message = 'Produto cadastrado com sucesso.';
        this.form.reset({ stock: 0 });
      },

      error: error => {
        this.loading = false;

        this.message =
          error.error?.message ??
          'Erro ao cadastrar produto.';
        
        this.cdr.detectChanges();
      }
    });
  }
}