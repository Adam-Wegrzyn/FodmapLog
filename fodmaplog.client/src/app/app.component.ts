import { Component, OnInit } from '@angular/core';
import { MsalBroadcastService, MsalService } from '@azure/msal-angular';
import { EventMessage, EventType } from '@azure/msal-browser';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent implements OnInit {
  title = 'fodmaplog.client';
  isAuthenticated: boolean = false;
  username: string = '';

  constructor(
    private msalService: MsalService,
    private msalBroadcastService: MsalBroadcastService,
  ) {}

 ngOnInit(): void {
  // Process the login response after redirect
  this.msalService.instance.handleRedirectPromise().then((response) => {
    if (response && response.account) {
      // Set the active account after login
      this.msalService.instance.setActiveAccount(response.account);
      console.log('Login successful, active account set:', response.account.username);
      this.updateAuthenticationState();
    }
  });

  // Listen for MSAL initialization and redirect events
  this.msalBroadcastService.msalSubject$
    .pipe(
      filter((event: EventMessage) => event.eventType === EventType.HANDLE_REDIRECT_END || event.eventType === EventType.INITIALIZE_END)
    )
    .subscribe(() => {
      console.log('MSAL initialized or redirect handled.');
      this.updateAuthenticationState();
    });

  // Check for an active account on app load
  this.updateAuthenticationState();
  
    // console.log("initializing MSAL...");
    // console.log(this.msalService.instance.getActiveAccount()?.name)
    // console.log(this.msalService.instance.getAllAccounts())
    // if(this.msalService.instance.getActiveAccount()) {
    //   this.isAuthenticated = true;
    // }

  }

  private updateAuthenticationState(): void {
    const activeAccount = this.msalService.instance.getActiveAccount();
    if (activeAccount) {
      this.isAuthenticated = true;
      this.username = activeAccount.username || '';
      console.log('Authentication state updated. Active account:', this.username);
    } else {
      this.isAuthenticated = false;
      this.username = '';
      console.log('No active account found.');
    }
  }

  checkAccount(): void {
    const accounts = this.msalService.instance.getAllAccounts();
    this.msalService.instance.setActiveAccount(accounts[0]);
    const activeAccount = this.msalService.instance.getActiveAccount();
    console.log('Active account in checkAccount:', activeAccount?.name);
    if (activeAccount) {
      console.log('User is authenticated:', activeAccount.name);
    } else {
      console.log('User is not authenticated.');
    }
  }

  isLoggedIn(): boolean {
    return this.isAuthenticated;
  }

  logout(): void {
    if (!this.isAuthenticated) {
      console.error('Cannot log out. No active account or MSAL is not initialized.');
      return;
    }

    this.msalService.logout();
 }
}