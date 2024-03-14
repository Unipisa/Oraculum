import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ChatComponent } from './components/chat/chat.component';
import { LayoutComponent } from './components/layout/layout.component';
import { KnowledgeComponent } from './components/knowledge/knowledge.component';
import { KpiComponent } from './components/kpi/kpi.component';
import { ConfigComponent } from './components/config/config.component';
import { ChatExplainComponent } from './components/chatExplain/chatExplain.component';
import { LoginComponent } from './components/login/login.component';
import { AuthGuard } from './services/auth.guard';
import { SelectionComponent } from './components/selection/selection.component';
import { ProfileComponent } from './components/profile/profile.component';

const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'selection',
    component: SelectionComponent,
    canActivate: [AuthGuard],
  },
  {
    path: '',
    component: LayoutComponent,
    // canActivateChild: [AuthGuard],
    children: [
      {
        path: 'chat',
        component: ChatComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'explain',
        component: ChatExplainComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'knowledge',
        component: KnowledgeComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'kpi',
        component: KpiComponent,
        canActivate: [],
      },
      {
        path: 'config',
        component: ConfigComponent,
        canActivate: [],
      },
      {
        path: 'profile',
        component: ProfileComponent,
        canActivate: [],
      },
    ]
  },
  { path: '**', redirectTo: 'login'},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
