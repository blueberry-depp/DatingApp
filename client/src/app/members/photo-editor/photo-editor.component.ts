import {Component, Input, OnInit} from '@angular/core';
import {Member} from "../../_models/member";
import {FileUploader} from "ng2-file-upload";
import {environment} from "../../../environments/environment";
import {AccountService} from "../../_services/account.service";
import {User} from "../../_models/user";
import {take} from "rxjs";
import {MembersService} from "../../_services/members.service";
import {Photo} from "../../_models/photo";

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  // We're going to receive our member from the parent component.
  @Input() member!: Member
  uploader!: FileUploader
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl
  user!: User


  constructor(
    private accountService: AccountService,
    private memberService: MembersService,
  ) {
    // Get our user out of the observable.
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => this.user = user)

  }

  ngOnInit(): void {
    this.initiliazeUploader()
  }

  // Provide PhotoEditorComponent with this method so that we can set our drop zone inside the template.
  fileOverBase(event: any) {
    this.hasBaseDropZoneOver = event
  }

  initiliazeUploader() {
    this.uploader = new FileUploader({
      url: `${this.baseUrl}users/add-photo`,
      // Get our user from account controller.
      authToken: `Bearer ${this.user.token}`,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024 // 10mbs
    })

    this.uploader.onAfterAddingFile = (file) => {
      // We set it to false we don't need to because we're using the bearer token to send our credentials with this file.
      file.withCredentials = false
    }

    // After the upload has successfully.
    this.uploader.onSuccessItem = (item, response, status, headers) => {
      // Check if we got the response.
      if (response) {
        // Parse this response out of Json response.
        const photo: Photo = JSON.parse(response)
        // Add photo into photos array.
        this.member.photos.push(photo)
        // Update the image everywhere.
        if (photo.isMain) {
          this.user.photoUrl = photo.url
          this.member.photoUrl = photo.url
          this.accountService.setCurrentUser(this.user)


        }
      }
    }
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo.id).subscribe(() => {
      this.user.photoUrl = photo.url
      // Update both our current user observable,
      // and it's also going to update our photo inside local storage, because we're also setting that inside there as well,
      // and what that means, if we close up browser and then come back, it's going to be able to get the photo out of
      // what we're storing in there and use that to display on our nav bar.
      this.accountService.setCurrentUser(this.user)
      this.member.photoUrl = photo.url
      // we need to go through each of the member photos and switch to one that is main to false and set the photo that we have here to true.
      this.member.photos.forEach(p => {
        // Check to see if p is the main photo currently and set it to false.
        if (p.isMain) p.isMain = false
        // We'll look for p.id that matches the photo.id and will set it true because that's the one we're setting to the main photo.
        if (p.id === photo.id) p.isMain = true
      })
    })
  }

  // We don't need to worry about updating anything else because we don't allow the user to interact with the main or delete the main photo,
  // and we also don't need to worry about handling the errors because our intercept is taking care of this for us.
  deletePhoto(photoId: number) {
    // We don't get anything back, so we'll just add empty parentheses.
    this.memberService.deletePhoto(photoId).subscribe(() => {
      // Filters out all the other photos or more accurately, this returns an array of all the photos that are not equal to the photoId we're passing in here.
      this.member.photos = this.member.photos.filter(p => p.id !== photoId)
    })
  }


}
