docker system prune -f
docker run --name mongo -v /Users/Maxim/MongoDB:/data/db -p 27017:27017 -d mongo