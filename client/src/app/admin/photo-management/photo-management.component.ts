import { Component, inject, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Photo } from '../../_models/photo';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-photo-management',
  imports: [NgFor],
  templateUrl: './photo-management.component.html',
  styleUrl: './photo-management.component.css'
})
export class PhotoManagementComponent implements OnInit {

  ngOnInit(): void {
    this.getPhotosForApproval();
  }
  adminService = inject(AdminService);
  photos: Photo[] = []; // Adjust the type based on your API response

  getPhotosForApproval() {
    this.adminService.getPhotosForApproval().subscribe({
      next: (photos) => {
        this.photos = photos.map(photo => ({
          ...photo,
          photoUrl: photo.url
        }));
        console.log('Photos for approval:', this.photos); 
      },
      error: (error) => {
        console.error('Error fetching photos for approval', error);
      }
    });
  }

  approvePhoto(photoId: number) {
    this.adminService.approvePhoto(photoId).subscribe({
      next: () => {
        console.log('Photo approved successfully');
        this.getPhotosForApproval(); // Refresh the list after approval
      },
      error: (error) => {
        console.error('Error approving photo', error);
      }
    });
  }

  rejectPhoto(photoId: number) {
    this.adminService.rejectPhoto(photoId).subscribe({
      next: () => {
        console.log('Photo rejected successfully');
        this.getPhotosForApproval(); // Refresh the list after rejection
      },
      error: (error) => {
        console.error('Error rejecting photo', error);
      }
    });
  }

}
