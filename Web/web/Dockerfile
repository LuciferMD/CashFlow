FROM node:22.12-alpine

WORKDIR /web

COPY package.json .

RUN npm install

COPY . .

EXPOSE 5173

CMD [ "npm", "run", "dev", "--", "--host" ]
