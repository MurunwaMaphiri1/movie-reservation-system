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
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=MovieDB;Username=postgres;Password=yourpassword"
}
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
