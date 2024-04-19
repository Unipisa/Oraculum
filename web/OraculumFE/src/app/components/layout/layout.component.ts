import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { AuthService } from 'src/app/services/auth.service';
import language from 'src/app/config/language';
import {
  trigger,
  state,
  style,
  animate,
  transition,
} from '@angular/animations';
import { DeviceDetectorService } from 'ngx-device-detector';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
  animations: [
    trigger('slideAnimation', [
      transition(':increment', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate(
          '300ms ease-out',
          style({ transform: 'translateX(0)', opacity: 1 })
        ),
      ]),
      transition(':decrement', [
        style({ transform: 'translateX(-100%)', opacity: 0 }),
        animate(
          '300ms ease-out',
          style({ transform: 'translateX(0)', opacity: 1 })
        ),
      ]),
    ]),
  ],
})
export class LayoutComponent implements OnInit, OnDestroy {
  title = '';
  private titleSubscription: Subscription = new Subscription();
  private botsSubscription: Subscription = new Subscription();
  language = language;
  activeTabIndex = 0;
  noHome: boolean = false;

  tabLinks = [
    { label: language.chat, url: '/chat' },
    { label: language.explain, url: '/explain' },
    { label: language.knowledge, url: '/knowledge' },
    // { label: 'KPI', url: '/kpi' },
    // { label: 'Config', url: '/config' }
  ];
  tabLinksMobile = [{ label: language.chat, url: '/chat' }];

  menuLinks = [{ label: 'Profilo', url: '/profile', icon: 'person' }];

  constructor(
    private authService: AuthService,
    private deviceService: DeviceDetectorService
  ) {
    if (this.deviceService.isMobile()) {
      this.tabLinks = this.tabLinksMobile;
    }
  }

  ngOnInit() {
    this.titleSubscription = this.authService.currentTitle.subscribe(
      (title) => {
        this.title = title;
      }
    );
    this.botsSubscription = this.authService
      .getVisibleBotsCount()
      .subscribe((count) => {
        if (count === 1) {
          // remove home tab
          this.noHome = true;
          // If there is only one visible bot, remove the 'Le mie Sibyllae' option
          this.menuLinks = this.menuLinks.filter(
            (link) => link.label !== 'Le mie Sibyllae'
          );
        }
      });
  }
  setActiveTab(index: number) {
    this.activeTabIndex = index;
  }

  ngOnDestroy() {
    this.titleSubscription.unsubscribe();
    this.botsSubscription.unsubscribe();
  }

  logout() {
    this.authService.logout();
  }
}
