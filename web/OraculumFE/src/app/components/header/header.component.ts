import { Component } from '@angular/core';
import { ItHeaderComponent, ItNavBarComponent, ItIconComponent } from 'design-angular-kit';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [ItNavBarComponent, ItIconComponent, TranslateModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent extends ItHeaderComponent{

}
