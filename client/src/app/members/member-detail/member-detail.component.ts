import { Component, computed, inject, OnDestroy, OnInit, ViewChild, viewChild } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoModule, TimeagoPipe } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';
import { PresenceService } from '../../_services/presence.service';
import { AccountService } from '../../_services/accounts.service';
import { HubConnectionState } from '@microsoft/signalr';
import { LikesService } from '../../_services/likes.service';

@Component({
  selector: 'app-member-detail',
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit, OnDestroy {
  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent;
  presenceService = inject(PresenceService);
  private messageService = inject(MessageService);
  private accountService = inject(AccountService);
  private likeService = inject(LikesService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  member: Member = {} as Member;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;

  hasLiked = computed(() => this.likeService.likeIds().includes(this.member.id));

  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        console.log('Member data:', this.member); // Debugging: Log the member data

        // Map photos to the images array as ImageItem instances
        this.images = this.member.photos?.map(photo => 
          new ImageItem({ src: photo.photoUrl, thumb: photo.photoUrl })
        ) || [];
      }
    });

    this.route.paramMap.subscribe({
      next: _ => this.onRouteParamsChange()
    });

    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab']);
      }
    });
  }
  
  selectTab(heading: string){
    if(this.memberTabs){
      const messageTab = this.memberTabs.tabs.find(x => x.heading == heading);
      if(messageTab) messageTab.active = true;
    }
  }

  onRouteParamsChange(){
    const user = this.accountService.currentUser();
    if(!user) return;
    if(this.messageService.hubConnection?.state === HubConnectionState.Connected && this.activeTab?.heading === 'Messages'){
      this.messageService.hubConnection.stop().then(() => {
        this.messageService.createHubConnection(user, this.member.username);
      })
    }
  }

  onTabActivated(data: TabDirective){
    this.activeTab = data;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {tab: this.activeTab.heading},
      queryParamsHandling:'merge'

    })
    if(this.activeTab?.heading === 'Messages' && this.member){
      const user = this.accountService.currentUser();
      if(!user) return;
      this.messageService.createHubConnection(user, this.member.username);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }

  toggleLike(){
    this.likeService.toggleLike(this.member.id).subscribe({
      next: () => {
        if(this.hasLiked()){
          this.likeService.likeIds.update(ids => ids.filter(x => x !== this.member.id))
        }
        else{
          this.likeService.likeIds.update(ids => [...ids, this.member.id])
        }
      }
    })
  }

  
}
