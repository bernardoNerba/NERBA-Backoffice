import { Injectable } from '@angular/core';
import { BsModalService, BsModalRef, ModalOptions } from 'ngx-bootstrap/modal';

@Injectable({
  providedIn: 'root',
})
export class CustomModalService {
  constructor(private modalService: BsModalService) {}

  show<T = any>(
    content: any,
    config?: ModalOptions<T>
  ): BsModalRef<T> {
    const defaultConfig: ModalOptions<T> = {
      ...config,
      backdrop: 'static',
      keyboard: false,
    };

    return this.modalService.show(content, defaultConfig) as BsModalRef<T>;
  }
}
