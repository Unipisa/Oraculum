import { Component, OnInit } from '@angular/core';
import { ConfigService } from 'src/app/services/config.service';
import { SnackbarService } from 'src/app/services/snackbar.service';

@Component({
  selector: 'app-config',
  templateUrl: './config.component.html',
  styleUrls: ['./config.component.scss'],
})
export class ConfigComponent implements OnInit {
  constructor(private configService: ConfigService, private snackbarService: SnackbarService) { }

  ngOnInit() { }
  // You can manage your configuration data and methods here
  contentsForResponse: number = this.configService.contentsForResponse.getValue();
  totalContentsHome: number = this.configService.totalContentsHome.getValue();

  applyChanges() {
    this.configService.contentsForResponse.next(this.contentsForResponse);
    this.configService.totalContentsHome.next(this.totalContentsHome);
    this.snackbarService.showSnackbar('Impostazioni applicate', 'Chiudi', 2000);
  }
}
