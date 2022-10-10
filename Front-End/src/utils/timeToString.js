import { format, parseISO } from "date-fns";
import { id } from "date-fns/locale";

export default function timeToString(
  string_waktu = new Date().toISOString(),
  format_waktu = "dd-mm-yyyy"
) {
  return format(parseISO(string_waktu), format_waktu, {
    locale: id,
  });
}
