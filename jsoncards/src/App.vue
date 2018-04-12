<template>
  <div id="app">

    <section class="hero is-primary">
      <div class="hero-body">
        <div class="container">
          <h1 class="title">
            Pubmed Webcrawler Results
          </h1>
          <h2 class="subtitle">
            Visualized from the json dump
          </h2>
        </div>
      </div>
    </section>

    <div class="container">
      <div class="columns is-multiline">

        <div class="column is-12" v-for="yourcard in jsonData">
          <div class="tile is-vertical is-parent">
            <div class="tile is-child box">
              <p class="title">{{ yourcard.title }}</p>
              <!-- <p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin ornare magna eros, eu pellentesque tortor vestibulum ut. Maecenas non massa sem. Etiam finibus odio quis feugiat facilisis.</p> -->
              <p>{{ yourcard.abstractTxt }}</p>
            </div>
          </div>
        </div>
      </div>

    </div>

  </div>
</template>

<script>
export default {
  name: "app",
  data: {
    jsonData: []
  },
  created() {
    fetch("./dump.json")
      .then(res => res.json())
      .then(json => (this.jsonData = json));
  }
};
</script>

<style>
#app {
  font-family: "Avenir", Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: left;
  color: #2c3e50;
}

h1,
h2 {
  font-weight: normal;
}

ul {
  list-style-type: none;
  padding: 0;
}

li {
  display: inline-block;
  margin: 0 10px;
}

a {
  color: #42b983;
}
</style>
