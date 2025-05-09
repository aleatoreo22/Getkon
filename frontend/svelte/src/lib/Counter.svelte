<script lang="ts">
  import { callNative } from "../interop/methods";
  let count: number = $state(0);
  const increment = () => {
    count += 1;
    callNative("I'm JS", "I'm JS").then((x) => console.log(x));
    // sendMessageAsync();
  };
</script>

<button onclick={increment}>
  count is {count}
</button>
