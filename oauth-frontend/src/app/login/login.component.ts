import { Component, NgZone, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/Auth.service';
import { PromptMomentNotification } from 'google-one-tap';
import { CredentialResponse } from 'google-one-tap';
import { error } from 'console';
import { enviroment } from '../../enviroment';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private service: AuthService,
    private _ngZone: NgZone
  ) { }

  ngOnInit() {
    if (typeof window !== 'undefined') {
      //@ts-ignore
      window.onGoogleLibraryLoad = () => {
        //@ts-ignore
        google.accounts.id.initialize({
          client_id: enviroment.clientId,
          callback: this.handleCredentialResponse.bind(this),
          auto_select: false,
          cancel_on_tap_outside: true,
          use_fedcm_for_prompt: true
        });
        //@ts-ignore
        google.accounts.id.renderButton(
          document.getElementById("buttonDiv"),
          {theme: "outline", size: "large", width: 100 }
        );
        //@ts-ignore
        google.accounts.id.prompt((notification: PromptMomentNotification) => {});
      }
    }
  }


  async handleCredentialResponse(response: CredentialResponse){
    debugger;
    await this.service.LoginWithGoogle(response.credential).subscribe(
      x => {
        debugger;
        localStorage.setItem("token", x.token);
        this._ngZone.run(() => {
          this.router.navigate(['/logout']);
        })},
      (error: any) => {
        console.log(error);
      }
    )
  }
}
