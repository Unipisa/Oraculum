import { Component, OnInit } from '@angular/core';
import { AuthService } from 'src/app/services/auth.service';
import { BotInfo } from 'src/app/interfaces/botInfo';
import language from 'src/app/config/language';

@Component({
  selector: 'app-selection',
  templateUrl: './selection.component.html',
  styleUrls: ['./selection.component.scss']
})
export class SelectionComponent implements OnInit {
  language = language;
  myBots: any;
  constructor(private authService: AuthService) { }

  ngOnInit() {
    this.authService.getMyBots().subscribe({
      next: data => {
        this.myBots = data.filter((bot: BotInfo) => !bot.hidden);
        // Automatically select the bot if only one visible bot is present
        if (this.myBots.length === 1) {
          this.selectBot(this.myBots[0]);
        }
      },
      error: err => {
        console.log(err);
      }
    })
  }

  selectBot(b: BotInfo) {
    this.authService.selectBot(b)
    localStorage.setItem('selectedSibylla', b.id);
  }
}
