<script lang="ts">
  import { sendMessageThroughCSharp } from "../csharp/ExposedMethods";
  let count: number = $state(0);
  const increment = async () => {
    count += 1;
    let message = await sendMessageThroughCSharp("I'm JS");
    console.log(message);
  };
</script>

<button onclick={increment}>
  count is {count}
</button>
