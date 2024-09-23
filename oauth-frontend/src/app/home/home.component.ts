import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../services/Auth.service';
import { response } from 'express';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  authorizationCode = "";
  accessToken = "";
  refreshToken = "";
  eventForm: FormGroup;

  constructor(private route: ActivatedRoute, private authService: AuthService, private fb: FormBuilder) { }


  ngOnInit() {
    this.route.queryParams.subscribe(async params => {
      this.authorizationCode = params['code'];
      if (this.authorizationCode) {
        await this.exchangeCodeForToken(this.authorizationCode);
      }
    });
    this.eventForm = this.fb.group({
      summary: ['', Validators.required],
      description: ['', Validators.required],
      start: ['', Validators.required],
      end: ['', Validators.required]
    });
  }

  async exchangeCodeForToken(authorizationCode: string): Promise<void> {
    try {
      const response = await this.authService.exchangeCodeForToken(authorizationCode).toPromise();
      this.accessToken = response.access_token;
      this.refreshToken = response.refresh_token;
      console.log('Access Token:', this.accessToken);
      console.log('Refresh Token:', this.refreshToken);
    } catch (error) {
      console.error('Error exchanging code for token:', error);
    }
  }

  async refreshAccessToken(): Promise<void> {
    try {
      const response = await this.authService.refreshAccessToken(this.refreshToken).toPromise();
      console.log('New Access Token:', response.access_token);
    } catch (error) {
      console.error('Error refreshing token:', error);
    }
  }

  verify() {
    console.log('Refresh Token:', this.refreshToken);
  }

  async onSubmit(): Promise<void> {
    debugger;
    if (this.eventForm.valid) {
      const formValue = this.eventForm.value;
      const consulta = {
        ...formValue,
        start: {
          dateTime: new Date(formValue.start).toISOString(),
          timeZone: 'America/Sao_Paulo'
        },
        end: {
          dateTime: new Date(formValue.end).toISOString(),
          timeZone: 'America/Sao_Paulo'
        }
      };
      try {
        debugger;
        const response = await this.authService.createAppointment(consulta, this.accessToken).subscribe(
          response => {
            console.log('marcada:', response);
          }
        );
      } catch (error) {
        console.error('Error:', error);
      }
    }
  }
}

