import { Component, NgZone, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/Auth.service';
import { enviroment } from '../../enviroment';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css']
})
export class LogoutComponent implements OnInit {

  constructor(private router: Router, private service: AuthService, private _ngZone: NgZone) { }

  ngOnInit() {
  }

  public logout(){
    this.service.signOutexternal();
    this._ngZone.run(() => {
      this.router.navigate(['/']).then(() => window.location.reload());
    })
  }

  calendarOAuth() {
    const requestScope = `https://accounts.google.com/o/oauth2/v2/auth?` +
      `client_id=${enviroment.clientId}&` +
      `response_type=code&` +
      `state=hellothere&` +
      `scope=${'https://www.googleapis.com/auth/calendar+https://www.googleapis.com/auth/calendar.events'}&` +
      `redirect_uri=${'http://localhost:4200/home'}&` +
      `prompt=consent&` +
      `access_type=offline&` +
      `include_granted_scopes=true`;

    window.location.href = requestScope;
  }

}
