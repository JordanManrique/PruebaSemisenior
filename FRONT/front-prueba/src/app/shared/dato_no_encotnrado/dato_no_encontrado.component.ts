import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-dato-no-encontrado',
  standalone: true,
  imports: [RouterLink, MatButtonModule],
  template: `
  <div class="not-found">
    <h2>Dato no encontrado</h2>
    <p>La página que busca no existe.</p>
    <a mat-flat-button color="primary" routerLink="/samples">Volver al listado</a>
  </div>
  `,
  styles: `.not-found { text-align: center; padding: 48px 16px; }`
})
export class DatoNoEncontradoComponent {

}
