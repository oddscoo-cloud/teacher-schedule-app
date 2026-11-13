FROM node:20-alpine
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . ./

ARG REACT_APP_API_URL=http://localhost:5101/api
ENV REACT_APP_API_URL=$REACT_APP_API_URL
ENV HOST=0.0.0.0

EXPOSE 3000
CMD ["npm", "start"]
