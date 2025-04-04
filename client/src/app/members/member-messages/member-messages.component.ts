import { AfterViewChecked, Component, inject, input, OnInit, output, ViewChild, viewChild } from '@angular/core';
import { MessageService } from '../../_services/message.service';
import { TimeagoClock, TimeagoModule } from 'ngx-timeago';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  imports: [TimeagoModule, FormsModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css'
})
export class MemberMessagesComponent implements AfterViewChecked{
  @ViewChild('messageForm') messageForm?: NgForm;
  @ViewChild('scrollMe') scrollContainer?: any;
  messageService = inject(MessageService);
  username = input.required<string>();
  messageContent = '';

  sendMessage(){
    this.messageService.sendMessage(this.username(), this.messageContent).then(() => {
      this.messageContent = ''; // Clear the message content
      this.messageForm?.resetForm(); // Reset the form properly
      this.scrollToBottom(); // Scroll to the bottom after sending the message
    });
  }  

  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  private scrollToBottom(): void {
    if(this.scrollContainer){
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    }
  }
}
