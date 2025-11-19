# Personal Trainer Features - TODO List

## ✅ Completed Features

### Backend
- [x] Post entity with database integration
- [x] EF Core migration for Posts table
- [x] Post DTOs (Create, Update, Response, Summary)
- [x] CQRS Commands (Create, Update, Delete)
- [x] CQRS Queries (GetMyPosts, GetPostById, GetTrainerPosts)
- [x] REST API endpoints (5 total)
- [x] PT profile fields in User entity
- [x] PT profile update endpoint

### Frontend
- [x] AI workout page UI improvements (minimalist design)
- [x] PT profile tab in instructor dashboard
- [x] Public PT profile page at /trainer/[slug]
- [x] Student invitation system (backend + frontend)
- [x] Activation flow for invited students
- [x] Gyms Near Me page

---

## 🔄 In Progress

### Posts Management UI
- [ ] Add "Posts" tab to instructor dashboard
- [ ] Create post form (title, content, image, draft/publish)
- [ ] Posts list view (edit/delete actions)
- [ ] Markdown editor or rich text support

---

## 📋 Remaining Tasks

### 1. Posts Feed Display
- [ ] Show published posts on PT public profile page (`/trainer/[slug]`)
- [ ] Create posts feed component (reusable)
- [ ] Display posts on student home/dashboard
- [ ] Add "Read More" functionality for long posts

### 2. PT Search/Discovery Page
- [ ] Create `/trainers` page for students
- [ ] Search by location, specialization, name
- [ ] Filter options (specialization, price range)
- [ ] Grid/list view of trainer cards
- [ ] Link to public profiles

### 3. Workout Plan Marketplace
- [ ] Add `ForSale` and `Price` fields to WorkoutPlan entity
- [ ] Create marketplace endpoint to list plans for sale
- [ ] Frontend: `/marketplace` page
- [ ] Plan detail page with purchase button
- [ ] Payment integration (Stripe/PayPal)
- [ ] Copy plan to student's account on purchase

### 4. Video/Media Upload
- [ ] File upload endpoint (AWS S3, Azure Blob, or Cloudinary)
- [ ] Video upload for exercise demonstrations
- [ ] Image upload for posts and profiles
- [ ] Video player component
- [ ] Thumbnail generation

---

## 🎯 Recommended Next Steps

1. **Complete Posts UI** (30-45 min)
   - Add Posts tab with create/edit form
   - Markdown or rich text editor
   - List view with edit/delete

2. **Add Posts to Profile Pages** (20-30 min)
   - Display on `/trainer/[slug]`
   - Show on student dashboard

3. **PT Discovery Page** (1-2 hours)
   - Search and filter trainers
   - Professional directory

4. **Marketplace** (2-3 hours)
   - Most complex feature
   - Requires payment integration

---

## 📝 Notes

- All backend APIs for posts are complete and tested
- Database migrations are ready to run
- Frontend uses glass morphism design system
- All features use CQRS pattern with MediatR

## 🚀 To Resume:

In a new Claude Code session, simply say:
> "Continue with the PT features. Let's add the Posts tab to the instructor dashboard."

Or pick any task from the list above!
