import { useState } from "react";
import { format } from "date-fns";
import { tr } from "date-fns/locale";
import { DayPicker } from "react-day-picker";
import * as Popover from "@radix-ui/react-popover";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { cn } from "@/lib/utils";
import "react-day-picker/style.css";

interface DatePickerProps {
    value: string | null;
    onChange: (date: string | null) => void;
    placeholder?: string;
    disabled?: boolean;
}

export default function DatePicker({ value, onChange, placeholder = "Tarih seçin", disabled }: DatePickerProps) {
    const [open, setOpen] = useState(false);

    const selectedDate = value ? new Date(value) : undefined;

    const handleSelect = (date: Date | undefined) => {
        if (date) {
            // YYYY-MM-DD formatında döndür
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, "0");
            const day = String(date.getDate()).padStart(2, "0");
            onChange(`${year}-${month}-${day}`);
        } else {
            onChange(null);
        }
        setOpen(false);
    };

    return (
        <Popover.Root open={open} onOpenChange={setOpen}>
            <Popover.Trigger asChild disabled={disabled}>
                <button
                    type="button"
                    className={cn(
                        "w-full text-left text-sm py-0.5 bg-transparent outline-none",
                        !value && "text-gray-400",
                        disabled && "cursor-not-allowed opacity-50"
                    )}
                >
                    {selectedDate ? format(selectedDate, "d MMMM yyyy", { locale: tr }) : placeholder}
                </button>
            </Popover.Trigger>

            <Popover.Portal>
                <Popover.Content
                    className="z-50 bg-white rounded-xl shadow-2xl border border-gray-100 p-4 animate-in fade-in zoom-in-95"
                    sideOffset={8}
                    align="start"
                >
                    <DayPicker
                        mode="single"
                        selected={selectedDate}
                        onSelect={handleSelect}
                        locale={tr}
                        showOutsideDays
                        captionLayout="dropdown"
                        fromYear={1950}
                        toYear={new Date().getFullYear()}
                        classNames={{
                            root: "rdp-custom",
                            months: "flex flex-col",
                            month: "space-y-4",
                            month_caption: "flex justify-center items-center gap-2 mb-4",
                            caption_label: "hidden",
                            nav: "flex items-center gap-1",
                            button_previous: "p-1.5 rounded-lg hover:bg-gray-100 transition-colors text-gray-600",
                            button_next: "p-1.5 rounded-lg hover:bg-gray-100 transition-colors text-gray-600",
                            month_grid: "w-full border-collapse",
                            weekdays: "flex",
                            weekday: "text-gray-500 text-xs font-medium w-9 h-9 flex items-center justify-center",
                            week: "flex",
                            day: "w-9 h-9 text-sm p-0",
                            day_button: "w-full h-full flex items-center justify-center rounded-lg hover:bg-primary-yellow/20 transition-colors",
                            selected: "!bg-primary-yellow text-primary-dark font-bold rounded-lg",
                            today: "font-bold text-primary-yellow",
                            outside: "text-gray-300",
                            disabled: "text-gray-200 cursor-not-allowed",
                            hidden: "invisible",
                            dropdowns: "flex items-center gap-2",
                            dropdown: "px-2 py-1 text-sm rounded-lg border border-gray-200 bg-white cursor-pointer hover:border-primary-yellow focus:outline-none focus:border-primary-yellow",
                        }}
                        components={{
                            Chevron: ({ orientation }) =>
                                orientation === "left" ? <ChevronLeft size={16} /> : <ChevronRight size={16} />,
                        }}
                    />

                    <div className="flex justify-between items-center mt-4 pt-3 border-t border-gray-100">
                        <button
                            type="button"
                            onClick={() => handleSelect(undefined)}
                            className="text-xs text-gray-500 hover:text-gray-700 transition-colors"
                        >
                            Temizle
                        </button>
                        <button
                            type="button"
                            onClick={() => handleSelect(new Date())}
                            className="text-xs text-primary-yellow font-medium hover:text-[#FFC107] transition-colors"
                        >
                            Bugün
                        </button>
                    </div>

                    <Popover.Arrow className="fill-white" />
                </Popover.Content>
            </Popover.Portal>
        </Popover.Root>
    );
}

