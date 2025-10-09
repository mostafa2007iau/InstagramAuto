

//حذف پروژه وبارگذاری دوباره
rm -rf InstagramAuto
git clone https://github.com/mostafa2007iau/InstagramAuto.git

//ایجاد داکر
docker build -t instagramauto-backend .

//ایجاد سرویس با اجرای docker-compose.yml
	docker-compose pull
	docker-compose up -d --force-recreate

//مشاهده لاگ
docker logs -f insta_backend

//لاگ زنده آنلاین
docker logs --tail 100 -f insta_backend


//مشاهده داکرها
//برای دیدن کانتینرهای در حال اجرا:
docker ps

//برای دیدن همه‌ی کانتینرها (از جمله متوقف‌شده‌ها):
docker ps -a

//حذف تمام کانتینرهای متوقف:
docker container prune

//حذف همه‌ی کانتینرها (در حال اجرا و متوقف) با یک دستور:
docker rm $(docker ps -aq)


docker-compose down

docker-compose build

//
docker-compose build --no-cache

//
docker-compose up -d
