# Movie Reservation System

## 📌 Overview
The **Movie Reservation System** is a **.NET Core Web API** that allows users to browse movies, select showtimes, and book reservations securely. The API is designed with **JWT authentication**, **Entity Framework Core**, and **PostgreSQL** for data storage.

## 🛠 Tech Stack
- **Backend:** C#/ASP.NET Core
- **Database:** PostgreSQL
- **Authentication:** JWT
- **Frontend (Optional):** React.js

## 🚀 Features
- **User Authentication**: Secure login and registration using JWT.
- **Movie Management**: Add, update, delete, and list available movies.
- **Showtime Scheduling**: Manage showtimes for different movies.
- **Reservations**: Book, view, and cancel reservations.
- **Error Handling**: Proper validation and error messages.

---

## 📌 Setup Instructions

### **1. Clone the Repository**
```bash
git clone https://github.com/MurunwaMaphiri1/movie-reservation-system.git
cd movie-reservation-system
```

### **2. Configure the Database**
- Install **PostgreSQL** and create a database.
- Update the `appsettings.json` file with your database connection string:
```json
#PostgreSQL configuration
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=MovieReservationsSystemDB
DATABASE_USERNAME=postgres
DATABASE_PASSWORD=

#Configuration settings
JWT_SECRET_KEY=a5a41fe08f7a133c1e4db417f057286f475d9e82c83e6c7eece28c45a277398810f5f5dbd113cb012688e4af3b7f039992b7b7cf10e6094c29fa02b0046de6f323bdb323e74d4dd05a9ad0d6a9ea171e8a4e871a7f2067d00ec01800b109a708f37e0131be9bc06885792a7c655ccc1ec89dc17313406b68b027d97bb8840f049bc82220a25e744377cd391808045e3d772aff68ea63bac1dc9e5dfc98773898b0287723d8126469a95a694c6411089ae2f29b4981a648e45b07eff82971e61eaf66abb3b5aefc1c23f826563d0ef90f13fd902cbccdd9d79bca00582a23b6924bc418f74c022150a01573821055a7047dae84cdd36e66adc94353014d1c518f3ca1d79a95b0f42e5d73aa978b48473128b4b63921abe21907d6e9df95cc8e51dc5a85f81136977d5cce424ea8bebe91a0ca26ba5bef2c412be3ee07da63fce1da3a3fa0cbbbd9bb5989b5dabad37f1ad919ff026a863476104a0457ac7c713a8e0613fa2937f46170311b395eb287880570c2c3c2dada71e4c423dc5320a25a83ce4778901299280e86cf1e058ee5bec1932078c28dcfdefed8988f9e5d825b608c0b3bb7c374f69ed67ae173589aeae414db370a131c47a06309c9010f75060bd3e20280282aa64c685c5011274e17527b358654cfbd0cc6a18516680057cc572b8143b102983827568f1bb5dd8caea76bda21b53da73b3b36f4db5af216ba

DATABASE_ISSUER=https://localhost:7035
DATABASE_AUDIENCE=http://localhost:5173

STRIPE_SECRET_KEY=

STRIPE_WEBHOOK_SECRET=
```

### **3. Run Database Migrations**
```bash
dotnet ef database update
```

### **4. Run the API**
```bash
dotnet run
```
The API should now be running at `http://localhost:7035`.

---

## 📌 API Endpoints

### **1. Authentication**
#### 🔹 User Registration
**POST** `/api/user/register`
##### Request Body:
```json
{
  "fullname": "johndoe",
  "email": "johndoe@example.com",
  "password": "SecurePassword123"
}
```
##### Response:
```json
{
  "message": "User registered successfully."
}
```

#### 🔹 User Login
**POST** `/api/user/login`
##### Request Body:
```json
{
  "email": "johndoe@example.com",
  "password": "SecurePassword123"
}
```
##### Response:
```json
{
  "token": "your-jwt-token"
}
```

---

### **2. Movies** (Requires Authentication)
#### 🔹 Add a Movie
**POST** `/api/movies`
##### Request Body:
```json
{
  "title": "Haikyuu!! The Dumpster Battle",
  "description": "Despite a strong field, the Karasuno High volleyball team advances past the preliminary round of the Harutaka tournament in Miyagi prefecture to reach the third round.",
  "genres": ["Anime", "Soccer", "Animation", "Drama", "Sport", "Comedy"],
  "image": "https://m.media-amazon.com/images/M/MV5BZWFiNmRkY2MtN2ViZC00MzhhLWI0YWYtNzVlZWFmZDI3ZjE3XkEyXkFqcGc@._V1_FMjpg_UX1000_.jpg",
  "releaseDate": "2024-02-16",
  "duration": "1h 25m",
  "directedBy": "Susumu Mitsunaka",
  "actors": [
    "Ayumu Murase",
    "Kaito Ishikawa",
    "Yuki Kaji",
    "Yuichi Nakamura",
    "Koki Uchiyama"
  ],
  "ticketPrice": 100,
  "trailer": "https://www.youtube.com/watch?v=H51vnZt1ctU"
}
```
##### Response:
```json
{
  "title": "Haikyuu!! The Dumpster Battle",
  "description": "Despite a strong field, the Karasuno High volleyball team advances past the preliminary round of the Harutaka tournament in Miyagi prefecture to reach the third round.",
  "genres": ["Anime", "Soccer", "Animation", "Drama", "Sport", "Comedy"],
  "image": "https://m.media-amazon.com/images/M/MV5BZWFiNmRkY2MtN2ViZC00MzhhLWI0YWYtNzVlZWFmZDI3ZjE3XkEyXkFqcGc@._V1_FMjpg_UX1000_.jpg",
  "releaseDate": "2024-02-16",
  "duration": "1h 25m",
  "directedBy": "Susumu Mitsunaka",
  "actors": [
    "Ayumu Murase",
    "Kaito Ishikawa",
    "Yuki Kaji",
    "Yuichi Nakamura",
    "Koki Uchiyama"
  ],
  "ticketPrice": 100,
  "trailer": "https://www.youtube.com/watch?v=H51vnZt1ctU"
}
```

#### 🔹 Get All Movies
**GET** `/api/movies`
##### Response:
```json
[
{
  "title": "Haikyuu!! The Dumpster Battle",
  "description": "Despite a strong field, the Karasuno High volleyball team advances past the preliminary round of the Harutaka tournament in Miyagi prefecture to reach the third round.",
  "genres": ["Anime", "Soccer", "Animation", "Drama", "Sport", "Comedy"],
  "image": "https://m.media-amazon.com/images/M/MV5BZWFiNmRkY2MtN2ViZC00MzhhLWI0YWYtNzVlZWFmZDI3ZjE3XkEyXkFqcGc@._V1_FMjpg_UX1000_.jpg",
  "releaseDate": "2024-02-16",
  "duration": "1h 25m",
  "directedBy": "Susumu Mitsunaka",
  "actors": [
    "Ayumu Murase",
    "Kaito Ishikawa",
    "Yuki Kaji",
    "Yuichi Nakamura",
    "Koki Uchiyama"
  ],
  "ticketPrice": 100,
  "trailer": "https://www.youtube.com/watch?v=H51vnZt1ctU"
}
]
```

---

### **3. Showtimes** 
#### 🔹 Add a Showtime
**POST** `/api/timeslots/add-time-slot`
##### Request Body:
```json
{
  {
    "timeSlotId": 1,
    "timeSlot": "09:00:00"
  }
}
```
##### Response:
```json
{
  {
    "timeSlotId": 1,
    "timeSlot": "09:00:00"
  }
}
```

#### 🔹 Get All Showtimes
**GET** `/api/showtimes/`
##### Response:
```json
[
  {
    "timeSlotId": 1,
    "timeSlot": "09:00:00"
  },
  {
    "timeSlotId": 2,
    "timeSlot": "11:00:00"
  },
  {
    "timeSlotId": 3,
    "timeSlot": "13:45:00"
  },
  {
    "timeSlotId": 4,
    "timeSlot": "16:45:00"
  },
  {
    "timeSlotId": 5,
    "timeSlot": "19:45:00"
  }
]
```

---

### **4. Reservations** (Requires Authentication)
#### 🔹 Create a Reservation
**POST** `/api/moviereservations`
##### Request Body:
```json
{
  "showtimeId": 1,
  "seats": 2
}
```
##### Response:
```json
{
  "id": 1,
  "showtimeId": 1,
  "seats": 2,
  "status": "Confirmed"
}
```

#### 🔹 View User Reservations
**GET** `/api/moviereservations/`
##### Response:
```json
[
  {
    "id": 2,
    "userId": 1,
    "user": null,
    "movieId": 2,
    "movie": null,
    "reservationDate": "2025-03-06T00:00:00Z",
    "timeSlotId": 5,
    "timeSlot": null,
    "seatNumbers": [
      "F7"
    ]
  }
]
```

#### 🔹 Cancel a Reservation
**DELETE** `/api/moviereservations/delete/{id}`
##### Response 204 No content

---

## 📌 Future Improvements
- Add **Admin Role** for managing movies and reservations.

---
