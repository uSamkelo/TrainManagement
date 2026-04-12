import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrainPositionComponent } from './train-position.component';

describe('TrainPositionComponent', () => {
  let component: TrainPositionComponent;
  let fixture: ComponentFixture<TrainPositionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TrainPositionComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TrainPositionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
