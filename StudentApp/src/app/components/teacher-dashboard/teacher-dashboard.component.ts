import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Teacher } from '../../core/models';
import { TeacherStateService } from '../../core/services/state';
import { ClassGroupApiService, ClassGroupDto } from '../../core/services/http';

@Component({
  selector: 'app-teacher-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './teacher-dashboard.component.html',

  styleUrls: ['./teacher-dashboard.component.css']
})
export class TeacherDashboardComponent implements OnInit, OnDestroy {
  teacher: Teacher | null = null;
  classGroups: ClassGroupDto[] = [];
  loading = true;
  groupsLoading = false;
  error = '';
  private destroy$ = new Subject<void>();

  get initials(): string {
    if (!this.teacher) return '';
    return (this.teacher.firstName[0] ?? '') + (this.teacher.lastName[0] ?? '');
  }

  constructor(
    private teacherState: TeacherStateService,
    private classGroupApi: ClassGroupApiService
  ) {}

  ngOnInit(): void {
    this.teacherState.currentTeacher$
      .pipe(takeUntil(this.destroy$))
      .subscribe(teacher => {
        this.teacher = teacher;
        this.loading = false;
        if (teacher) this.loadClassGroups();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadClassGroups(): void {
    this.groupsLoading = true;
    this.classGroupApi.getAll()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: groups => {
          this.classGroups = groups;
          this.groupsLoading = false;
        },
        error: () => {
          this.groupsLoading = false;
          this.error = 'Failed to load class groups.';
        }
      });
  }
}
