// Export this as a class and this way we can give it some initial parameters that we can use. When we
// instantiate new instance of this class.
import {User} from "./user";

export class UserParams {
  gender!: string
  minAge = 18
  maxAge = 99
  pageNumber = 1
  pageSize = 3
  orderBy = 'lastActive'

  constructor(user: User) {
    this.gender = user.gender === 'female' ? 'male' : 'female'
  }
}
