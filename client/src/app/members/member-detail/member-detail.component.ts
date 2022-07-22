import {Component, OnInit, ViewChild} from '@angular/core';
import {MembersService} from "../../_services/members.service";
import {Member} from "../../_models/member";
import {ActivatedRoute} from "@angular/router";
import {NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions} from "@kolkov/ngx-gallery";
import {TabDirective, TabsetComponent} from "ngx-bootstrap/tabs";
import {MessageService} from "../../_services/message.service";
import {Message} from "../../_models/message";

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  // To get that memberTabs.
  // TabsetComponent is ngx-bootstrap component.
  // When component is constructed, then this ViewChild is undefined because it's not faster than ngInit, so we must use route resolvers,
  // route resolvers allow us to get access to the data before the component is constructed, and that means that we can have everything
  // we need when the component is constructed, and we're not going to see this tab undefined error.
  // static: is a dynamic version of ViewChild, it's able to react to changes in component,
  @ViewChild('memberTabs', {static: true}) memberTabs!: TabsetComponent
  member!: Member
  galleryOptions!: NgxGalleryOptions[];
  galleryImages!: NgxGalleryImage[];
  // Each one of our tabs in html component, we've got a tab directive specifically.
  activeTab!: TabDirective
  messages: Message[] = []

  constructor(
    private memberService: MembersService,
    // We're going to use the username to decide which user this is, and we need to access that particular user's profile.
    // And this is where we get access to the [queryParams] that we're passing that.
    private route: ActivatedRoute,
    private messageService: MessageService

  ) { }

  // Everything that's going on in here is synchronous, everything's going to happen one after the other, and there's no waiting for something.
  ngOnInit(): void {
    // Get the member from route resolver.
    // data: the data property that gives us the resolved data of the route.
    this.route.data.subscribe(data => {
      // And now we guarantee that our route is going to have this member inside it.
      this.member = data['member']
    })


    this.route.queryParams.subscribe(params => {
      // Check to see if we have something in queryParams.tab
      params['tab'] ? this.selectTab(params['tab']) : this.selectTab(0)
    })

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ]

    this.galleryImages = this.getImage()
  }

  // Get the images outside our member objects.
  getImage(): NgxGalleryImage[] {
    const imageUrls = []
    // Loop over all member photos.
    for (const photo of this.member.photos) {
      // Push the images that we have inside our member photos into imageUrls array.
      imageUrls.push({
        // photo? is optional.
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url,
      })
    }
    return imageUrls
  }

  // Bring the functionality for the loading of messages in detail component so that we know if that tabs been clicked,
  // And because we know that the messages are going to be loaded from this component anyway, because the
  // members message is a child component.
  loadMessages() {
    this.messageService.getMessageThread(this.member.username).subscribe(messages => {
      console.log(messages)
      this.messages = messages
    })
  }

  selectTab(tabId: number) {
    // memberTabs: we can use memberTabs to select the tab specifically.
    this.memberTabs.tabs[tabId].active = true
  }

  onTabActivated(data: TabDirective) {
    // We have access to the information inside that tabs.
    this.activeTab = data
    // check the message heading and messages length because if they're switching between the tabs of a user, and we already
    // have messages loaded inside this component, then obviously we're not going to dispose of them and then reload them again. We'll just use the same ones.
    if (this.activeTab.heading === 'Messages' && this.messages.length === 0) {
      this.loadMessages()
    }
  }

}
