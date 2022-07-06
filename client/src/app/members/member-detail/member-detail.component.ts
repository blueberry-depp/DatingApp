import { Component, OnInit } from '@angular/core';
import {MembersService} from "../../_services/members.service";
import {Member} from "../../_models/member";
import {HttpClient} from "@angular/common/http";
import {ActivatedRoute} from "@angular/router";
import {NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions} from "@kolkov/ngx-gallery";

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  member!: Member
  galleryOptions!: NgxGalleryOptions[];
  galleryImages!: NgxGalleryImage[];

  constructor(
    private memberService: MembersService,
    // We're going to use the username to decide which user this is and we need to access that particular user's profile.
    private route: ActivatedRoute

  ) { }

  ngOnInit(): void {
    // Everything that's going on in here is synchronous, everything's going to happen one after the other, and there's no waiting for something.

    this.loadMember()

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];

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

  loadMember() {
    // username: url parameter in app-routing-module
    // @ts-ignore
    this.memberService.getMember(this.route.snapshot.paramMap.get('username')).subscribe(member => {
      this.member = member
      this.galleryImages = this.getImage()

    })
  }

}
