import {Component, OnInit} from '@angular/core';
import {Router} from "@angular/router";

@Component({
  selector: 'app-server-error',
  templateUrl: './server-error.component.html',
  styleUrls: ['./server-error.component.css']
})
export class ServerErrorComponent implements OnInit {
  error: any

  // There's only one place we can get hold of the router estate,
  // we can't access it inside the OnInit, we can only access the state inside the constructor here
  constructor(private router: Router,
  ) {
    const navigation = this.router.getCurrentNavigation()
    // Check the navigation, but we're going to be safe here because we don't know if we're going to have any of this information,
    // because what's going to happen as soon as the user refreshes their page, then we immediately lose whatever's inside this navigation state,
    // we only get it once when the route is activated, when we redirect the user to this server error page,
    // so we're going to be safe and we're going to use these question marks, which are referred to as optional chaining operators,
    // and what we can do is say that we want to check the extras and once again, we'll use optional chaining and then we want to check the state,
    // once again, optional chaining and then the error inside there.
    this.error = navigation?.extras?.state?.['error']

  }

  ngOnInit(): void {
  }

}
