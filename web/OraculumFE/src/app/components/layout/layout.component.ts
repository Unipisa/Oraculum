import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuthService } from 'src/app/services/auth.service';
import language from 'src/app/config/language';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
})
export class LayoutComponent implements OnInit {
  title = ''
  private titleSubscription: Subscription = new Subscription();
  private botsSubscription: Subscription = new Subscription();

  tabLinks = [
    { label: language.chat, url: '/chat' },
    { label: language.explain, url: '/explain' },
    { label: language.knowledge, url: '/knowledge' },
    // { label: 'KPI', url: '/kpi' },
    // { label: 'Config', url: '/config' }
  ];

  menuLinks = [
    { label: 'Profilo', url: '/profile', icon: 'person' },
    { label: 'Le mie Sibyllae', url: '/selection', icon: 'dashboard' },
  ];

  constructor(private authService: AuthService) { }

  ngOnInit() {
    this.titleSubscription = this.authService.currentTitle.subscribe(
      title => {
        this.title = title;
      }
    );
    this.botsSubscription = this.authService.getVisibleBotsCount().subscribe(count => {
      if (count === 1) {
        // If there is only one visible bot, remove the 'Le mie Sibyllae' option
        this.menuLinks = this.menuLinks.filter(link => link.label !== 'Le mie Sibyllae');
      }
    });
  }

  ngOnDestroy() {
    this.titleSubscription.unsubscribe();
    this.botsSubscription.unsubscribe();
  }

  logout() {
    this.authService.logout();
  }
}
