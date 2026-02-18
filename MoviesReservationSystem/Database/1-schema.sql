-- Users table
CREATE TABLE "Users" (
                         "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                         "FullName" TEXT NOT NULL,
                         "Email" TEXT NOT NULL,
                         "Password" TEXT NOT NULL,
                         "Role" TEXT NOT NULL DEFAULT 'User'
);

-- Movies table
CREATE TABLE "Movies" (
                          "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                          "Title" TEXT NOT NULL,
                          "Description" TEXT NOT NULL,
                          "Genres" TEXT[] NOT NULL DEFAULT '{}',
                          "Image" TEXT NOT NULL,
                          "ReleaseDate" DATE NOT NULL,
                          "Duration" TEXT NOT NULL DEFAULT 'N/A',
                          "DirectedBy" TEXT NOT NULL,
                          "Actors" TEXT[] NOT NULL DEFAULT '{}',
                          "TicketPrice" BIGINT NOT NULL,
                          "Trailer" TEXT NOT NULL
);

-- TimeSlots table
CREATE TABLE "TimeSlots" (
                             "TimeSlotId" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                             "TimeSlot" TIME NOT NULL
);

-- Employees table
CREATE TABLE "Employees" (
                             "EmployeeId" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                             "FullName" TEXT NOT NULL,
                             "Email" TEXT NOT NULL,
                             "Password" TEXT NOT NULL,
                             "PhoneNumber" TEXT NOT NULL,
                             "Role" TEXT NOT NULL
);

-- MovieReservations table (with foreign keys)
CREATE TABLE "MovieReservations" (
                                     "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                                     "UserId" INTEGER NOT NULL,
                                     "ReservationCode" TEXT,
                                     "MovieId" INTEGER NOT NULL,
                                     "PaymentId" TEXT,
                                     "ReservationDate" DATE NOT NULL,
                                     "TimeSlotId" INTEGER NOT NULL,
                                     "SeatNumbers" TEXT[] NOT NULL DEFAULT '{}',

    -- Foreign key constraints
                                     CONSTRAINT "FK_MovieReservations_Users_UserId"
                                         FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE,
                                     CONSTRAINT "FK_MovieReservations_Movies_MovieId"
                                         FOREIGN KEY ("MovieId") REFERENCES "Movies"("Id") ON DELETE CASCADE,
                                     CONSTRAINT "FK_MovieReservations_TimeSlots_TimeSlotId"
                                         FOREIGN KEY ("TimeSlotId") REFERENCES "TimeSlots"("TimeSlotId") ON DELETE CASCADE
);

-- Create indexes for foreign keys to improve query performance
CREATE INDEX "IX_MovieReservations_UserId" ON "MovieReservations"("UserId");
CREATE INDEX "IX_MovieReservations_MovieId" ON "MovieReservations"("MovieId");
CREATE INDEX "IX_MovieReservations_TimeSlotId" ON "MovieReservations"("TimeSlotId");